-- =====================================================
-- AgroBoost Database Scripts - Views and Stored Procedures
-- Unit V Evidence: ADO.NET, Stored Procedures, Views, Complex JOINs
-- =====================================================

-- =====================================================
-- VIEW: vw_SellerAnalytics
-- Used by sp_SellerDashboardAnalytics for instant summary stats
-- =====================================================
IF OBJECT_ID('vw_SellerAnalytics', 'V') IS NOT NULL
    DROP VIEW vw_SellerAnalytics;
GO

CREATE VIEW vw_SellerAnalytics AS
SELECT 
    p.SellerId,
    u.OrganizationName,
    COUNT(DISTINCT p.Id) AS TotalProducts,
    SUM(CASE WHEN p.IsActive = 1 THEN 1 ELSE 0 END) AS ActiveProducts,
    SUM(p.StockQty) AS TotalInventoryUnits,
    COUNT(DISTINCT o.Id) AS TotalOrders,
    SUM(CASE WHEN o.Status = 'Delivered' THEN 1 ELSE 0 END) AS CompletedOrders,
    SUM(CASE WHEN o.Status = 'Delivered' THEN o.TotalAmount ELSE 0 END) AS TotalRevenue,
    AVG(CASE WHEN o.Status = 'Delivered' THEN o.TotalAmount END) AS AvgOrderValue,
    MAX(o.OrderDate) AS LastOrderDate
FROM Products p
LEFT JOIN AspNetUsers u ON p.SellerId = u.Id
LEFT JOIN OrderItems oi ON p.Id = oi.ProductId
LEFT JOIN Orders o ON oi.OrderId = o.Id
GROUP BY p.SellerId, u.OrganizationName;
GO

-- =====================================================
-- STORED PROCEDURE 1: sp_AdvancedProductSearch
-- Complex search with dynamic WHERE, multiple JOINs, pagination
-- Used by ProductSearchRepository (ADO.NET) and ProductsApiController
-- =====================================================
IF OBJECT_ID('sp_AdvancedProductSearch', 'P') IS NOT NULL
    DROP PROCEDURE sp_AdvancedProductSearch;
GO

CREATE PROCEDURE sp_AdvancedProductSearch
    @Keyword NVARCHAR(200) = NULL,
    @CategoryId INT = NULL,
    @MinPrice DECIMAL(18,2) = NULL,
    @MaxPrice DECIMAL(18,2) = NULL,
    @SellerType NVARCHAR(10) = NULL, -- 'SHG' or 'FPG'
    @PageNumber INT = 1,
    @PageSize INT = 12,
    @SortBy NVARCHAR(50) = 'CreatedAt',
    @SortDir NVARCHAR(4) = 'DESC'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    DECLARE @Sql NVARCHAR(MAX), @Params NVARCHAR(MAX);

    -- Dynamic ORDER BY (safe list to prevent SQL injection)
    -- NOTE: there is no rating column in the Products table, so 'Rating' falls back
    -- to name ordering instead of referencing a non-existent column.
    DECLARE @OrderBy NVARCHAR(100) = CASE @SortBy
        WHEN 'Price' THEN 'p.BasePrice'
        WHEN 'Name' THEN 'p.Name'
        WHEN 'Rating' THEN 'p.Name'
        ELSE 'p.CreatedAt' END + ' ' + CASE WHEN @SortDir = 'ASC' THEN 'ASC' ELSE 'DESC' END;

    SET @Sql = N'
        SELECT p.Id, p.Name, p.Description, p.Unit, p.BasePrice, p.StockQty,
               p.Images, p.Certifications, c.Name AS CategoryName,
               u.FullName AS SellerName, u.OrganizationName, u.OrganizationType,
               u.District, u.State
        FROM Products p
        INNER JOIN Categories c ON p.CategoryId = c.Id
        INNER JOIN AspNetUsers u ON p.SellerId = u.Id
        WHERE p.IsActive = 1 AND u.IsActive = 1
          AND (@Keyword IS NULL OR p.Name LIKE ''%'' + @Keyword + ''%'' OR p.Description LIKE ''%'' + @Keyword + ''%'')
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@MinPrice IS NULL OR p.BasePrice >= @MinPrice)
          AND (@MaxPrice IS NULL OR p.BasePrice <= @MaxPrice)
          AND (@SellerType IS NULL OR u.OrganizationType = @SellerType)
        ORDER BY ' + @OrderBy + '
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*) FROM Products p
        INNER JOIN AspNetUsers u ON p.SellerId = u.Id
        WHERE p.IsActive = 1 AND u.IsActive = 1
          AND (@Keyword IS NULL OR p.Name LIKE ''%'' + @Keyword + ''%'' OR p.Description LIKE ''%'' + @Keyword + ''%'')
          AND (@CategoryId IS NULL OR p.CategoryId = @CategoryId)
          AND (@MinPrice IS NULL OR p.BasePrice >= @MinPrice)
          AND (@MaxPrice IS NULL OR p.BasePrice <= @MaxPrice)
          AND (@SellerType IS NULL OR u.OrganizationType = @SellerType);';

    SET @Params = N'@Keyword NVARCHAR(200), @CategoryId INT, @MinPrice DECIMAL(18,2), @MaxPrice DECIMAL(18,2), @SellerType NVARCHAR(10), @Offset INT, @PageSize INT';
    EXEC sp_executesql @Sql, @Params, @Keyword, @CategoryId, @MinPrice, @MaxPrice, @SellerType, @Offset, @PageSize;
END;
GO

-- =====================================================
-- STORED PROCEDURE 2: sp_SellerDashboardAnalytics
-- Uses vw_SellerAnalytics view + window functions for time-series
-- Returns 4 result sets: Summary, Monthly Revenue, Top Products, Order Funnel
-- =====================================================
IF OBJECT_ID('sp_SellerDashboardAnalytics', 'P') IS NOT NULL
    DROP PROCEDURE sp_SellerDashboardAnalytics;
GO

CREATE PROCEDURE sp_SellerDashboardAnalytics
    @SellerId NVARCHAR(450),
    @MonthsBack INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: Summary from View (instant)
    SELECT * FROM vw_SellerAnalytics WHERE SellerId = @SellerId;
    
    -- Result Set 2: Monthly Revenue Trend (window function for cumulative)
    SELECT 
        FORMAT(o.OrderDate, 'yyyy-MM') AS Month,
        SUM(o.TotalAmount) AS Revenue,
        COUNT(*) AS Orders,
        SUM(SUM(o.TotalAmount)) OVER (ORDER BY FORMAT(o.OrderDate, 'yyyy-MM') ROWS UNBOUNDED PRECEDING) AS CumulativeRevenue
    FROM Orders o
    WHERE o.SellerId = @SellerId 
      AND o.Status = 'Delivered'
      AND o.OrderDate >= DATEADD(MONTH, -@MonthsBack, GETUTCDATE())
    GROUP BY FORMAT(o.OrderDate, 'yyyy-MM')
    ORDER BY Month;
    
    -- Result Set 3: Top 5 Products by Revenue
    SELECT TOP 5 p.Name, SUM(oi.TotalPrice) AS ProductRevenue, COUNT(*) AS UnitsSold
    FROM OrderItems oi
    INNER JOIN Products p ON oi.ProductId = p.Id
    INNER JOIN Orders o ON oi.OrderId = o.Id
    WHERE p.SellerId = @SellerId AND o.Status = 'Delivered'
    GROUP BY p.Name
    ORDER BY ProductRevenue DESC;
    
    -- Result Set 4: Order Funnel
    SELECT Status, COUNT(*) AS Count
    FROM Orders WHERE SellerId = @SellerId
    GROUP BY Status;
END;
GO

-- =====================================================
-- STORED PROCEDURE 3: sp_AdminPlatformReport
-- Platform-wide analytics with category performance and geographic distribution
-- Returns 3 result sets: Summary, Category Performance, Geographic Distribution
-- =====================================================
IF OBJECT_ID('sp_AdminPlatformReport', 'P') IS NOT NULL
    DROP PROCEDURE sp_AdminPlatformReport;
GO

CREATE PROCEDURE sp_AdminPlatformReport
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Result Set 1: Summary Stats
    SELECT 
        (SELECT COUNT(*) FROM AspNetUsers WHERE IsActive = 1) AS TotalUsers,
        (SELECT COUNT(*) FROM AspNetUsers WHERE UserType = 'Seller' AND IsActive = 1) AS TotalSellers,
        (SELECT COUNT(*) FROM AspNetUsers WHERE UserType = 'Buyer' AND IsActive = 1) AS TotalBuyers,
        (SELECT COUNT(*) FROM AspNetUsers WHERE UserType = 'Seller' AND IsVerified = 1 AND IsActive = 1) AS VerifiedSellers,
        (SELECT COUNT(*) FROM AspNetUsers WHERE UserType = 'Seller' AND IsVerified = 0 AND IsActive = 1) AS PendingVerification,
        (SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS TotalProducts,
        (SELECT COUNT(*) FROM Products WHERE IsActive = 1 AND StockQty > 0) AS ActiveProducts,
        (SELECT COUNT(*) FROM Orders) AS TotalOrders,
        (SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE Status = 'Delivered') AS TotalRevenue;
    
    -- Result Set 2: Category Performance
    SELECT 
        c.Name AS CategoryName,
        COUNT(DISTINCT p.Id) AS ProductCount,
        COUNT(DISTINCT o.Id) AS OrderCount,
        ISNULL(SUM(CASE WHEN o.Status = 'Delivered' THEN oi.TotalPrice END), 0) AS Revenue
    FROM Categories c
    LEFT JOIN Products p ON c.Id = p.CategoryId AND p.IsActive = 1
    LEFT JOIN OrderItems oi ON p.Id = oi.ProductId
    LEFT JOIN Orders o ON oi.OrderId = o.Id AND o.Status = 'Delivered'
    WHERE c.IsActive = 1
    GROUP BY c.Name
    ORDER BY Revenue DESC;
    
    -- Result Set 3: Geographic Distribution (by State)
    SELECT 
        u.State,
        COUNT(DISTINCT u.Id) AS SellerCount,
        COUNT(DISTINCT p.Id) AS ProductCount
    FROM AspNetUsers u
    LEFT JOIN Products p ON u.Id = p.SellerId AND p.IsActive = 1
    WHERE u.UserType = 'Seller' AND u.IsActive = 1 AND u.State IS NOT NULL
    GROUP BY u.State
    ORDER BY SellerCount DESC;
END;
GO