import json
import requests
from pathlib import Path

with open("product_images.json", "r") as f:
    data = json.load(f)

category_map = {
    "Grains & Cereals": "grains-cereals.jpg",
    "Processed Foods": "processed-foods.jpg",
    "Pulses & Lentils": "pulses-lentils.jpg",
}

output_dir = Path(r"D:\Model\ASP.net\AgroBoost.MVC\wwwroot\images\categories")

for category, filename in category_map.items():
    if category in data and data[category]:
        url = data[category][0]["url"]
        try:
            print(f"Downloading {category}...")
            response = requests.get(url, timeout=30)
            response.raise_for_status()
            
            filepath = output_dir / filename
            filepath.write_bytes(response.content)
            print(f"  Saved: {filepath} ({len(response.content)} bytes)")
        except Exception as e:
            print(f"  Failed: {e}")

print("\nDone!")