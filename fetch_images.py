import requests
import json
import os
from typing import List, Dict

UNSPLASH_ACCESS_KEY = os.getenv("UNSPLASH_ACCESS_KEY")
PEXELS_API_KEY = os.getenv("PEXELS_API_KEY")

CATEGORIES = {
    "Dairy Products": ["milk cheese yogurt dairy", "dairy products farm"],
    "Fruits & Vegetables": ["fresh fruits vegetables produce", "organic fruits vegetables market"],
    "Grains & Cereals": ["wheat rice oats barley grains", "cereal grains harvest"],
    "Oilseeds & Oils": ["olive oil sunflower oil seeds", "cooking oils bottles"],
    "Organic Products": ["organic vegetables certification", "organic food label"],
    "Processed Foods": ["packaged snacks canned goods frozen meals", "processed food products"],
    "Pulses & Lentils": ["lentils beans chickpeas pulses", "legumes varieties"],
    "Spices & Condiments": ["spices herbs jars powder", "condiments spices rack"],
}

def search_unsplash(query: str, per_page: int = 5) -> List[Dict]:
    if not UNSPLASH_ACCESS_KEY:
        return []
    url = "https://api.unsplash.com/search/photos"
    params = {
        "query": query,
        "per_page": per_page,
        "orientation": "landscape",
        "order_by": "relevant",
    }
    headers = {"Authorization": f"Client-ID {UNSPLASH_ACCESS_KEY}"}
    r = requests.get(url, params=params, headers=headers, timeout=10)
    r.raise_for_status()
    data = r.json()
    return [
        {
            "url": photo["urls"]["regular"],
            "thumb": photo["urls"]["small"],
            "alt": photo.get("alt_description", ""),
            "photographer": photo["user"]["name"],
            "source": "Unsplash",
        }
        for photo in data.get("results", [])
    ]

def search_pexels(query: str, per_page: int = 5) -> List[Dict]:
    if not PEXELS_API_KEY:
        return []
    url = "https://api.pexels.com/v1/search"
    params = {"query": query, "per_page": per_page, "orientation": "landscape"}
    headers = {"Authorization": PEXELS_API_KEY}
    r = requests.get(url, params=params, headers=headers, timeout=10)
    r.raise_for_status()
    data = r.json()
    return [
        {
            "url": photo["src"]["large2x"],
            "thumb": photo["src"]["medium"],
            "alt": photo.get("alt", ""),
            "photographer": photo["photographer"],
            "source": "Pexels",
        }
        for photo in data.get("photos", [])
    ]

def fetch_all():
    results = {}
    for cat, queries in CATEGORIES.items():
        cat_images = []
        for q in queries:
            for fn in (search_unsplash, search_pexels):
                try:
                    imgs = fn(q, per_page=3)
                    cat_images.extend(imgs)
                    if len(cat_images) >= 5:
                        break
                except Exception as e:
                    print(f"  {fn.__name__} error for '{q}': {e}")
            if len(cat_images) >= 5:
                break
        results[cat] = cat_images[:5]
        print(f"{cat}: {len(results[cat])} images")
    return results

if __name__ == "__main__":
    print("Fetching images...")
    data = fetch_all()
    with open("product_images.json", "w", encoding="utf-8") as f:
        json.dump(data, f, indent=2, ensure_ascii=False)
    print("\nSaved to product_images.json")
    for cat, imgs in data.items():
        print(f"\n{cat}:")
        for i, img in enumerate(imgs, 1):
            print(f"  {i}. {img['url']}  ({img['source']} - {img['photographer']})")