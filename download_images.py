import json
import requests
from pathlib import Path

# Load the image data
with open("product_images.json", "r") as f:
    data = json.load(f)

# Map categories to existing filenames
category_map = {
    "Dairy Products": "dairy-products.jpg",
    "Fruits & Vegetables": "fruits-vegetables.jpg",
    "Grains & Cereals": "grains-cereals.jpg",
    "Oilseeds & Oils": "oilseeds-oils.jpg",
    "Organic Products": "organic-products.jpg",
    "Processed Foods": "processed-foods.jpg",
    "Pulses & Lentils": "pulses-lentils.jpg",
    "Spices & Condiments": "spices-condiments.jpg",
}

output_dir = Path(r"D:\Model\ASP.net\AgroBoost.MVC\wwwroot\images\categories")

for category, filename in category_map.items():
    if category in data and data[category]:
        url = data[category][0]["url"]  # Use first image
        try:
            print(f"Downloading {category}...")
            response = requests.get(url, timeout=30)
            response.raise_for_status()
            
            # Save as JPG
            filepath = output_dir / filename
            filepath.write_bytes(response.content)
            print(f"  Saved: {filepath} ({len(response.content)} bytes)")
        except Exception as e:
            print(f"  Failed: {e}")

print("\nDone!")