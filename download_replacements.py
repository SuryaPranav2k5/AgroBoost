import json
import requests
from pathlib import Path

# Replacement image URLs for the mismatched category images
new_images = {
    "Grains & Cereals": {
        "url": "https://images.pexels.com/photos/8545774/pexels-photo-8545774.jpeg",
        "filename": "grains-cereals.jpg",
    },
    "Pulses & Lentils": {
        "url": "https://images.pexels.com/photos/1633973/pexels-photo-1633973.jpeg",
        "filename": "pulses-lentils.jpg",
    },
}

output_dir = Path(r"D:\Model\ASP.net\AgroBoost.MVC\wwwroot\images\categories")

for category, info in new_images.items():
    try:
        print(f"Downloading {category}...")
        response = requests.get(info["url"], timeout=30)
        response.raise_for_status()
        filepath = output_dir / info["filename"]
        filepath.write_bytes(response.content)
        print(f"  Saved: {filepath} ({len(response.content)} bytes)")
    except Exception as e:
        print(f"  Failed: {e}")

print("\nDone!")