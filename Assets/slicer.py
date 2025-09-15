from PIL import Image
import os

# Border coordinates from CSS (top, right, bottom, left)
top, right, bottom, left = 50,50,50,50

img = Image.open("/home/enderdremurr/RiderProjects/RainbusToolbox/Assets/TextEditor.png")
w, h = img.size
base_name = "TextEditor"
output_dir = "/home/enderdremurr/RiderProjects/RainbusToolbox/Assets/"

# Make sure the directory exists
os.makedirs(output_dir, exist_ok=True)

# Corners
img.crop((0, 0, left, top)).save(os.path.join(output_dir, f"{base_name}TopLeft.png"))
img.crop((w-right, 0, w, top)).save(os.path.join(output_dir, f"{base_name}TopRight.png"))
img.crop((0, h-bottom, left, h)).save(os.path.join(output_dir, f"{base_name}BottomLeft.png"))
img.crop((w-right, h-bottom, w, h)).save(os.path.join(output_dir, f"{base_name}BottomRight.png"))

# Edges
img.crop((left, 0, w-right, top)).save(os.path.join(output_dir, f"{base_name}Top.png"))
img.crop((left, h-bottom, w-right, h)).save(os.path.join(output_dir, f"{base_name}Bottom.png"))
img.crop((0, top, left, h-bottom)).save(os.path.join(output_dir, f"{base_name}Left.png"))
img.crop((w-right, top, w, h-bottom)).save(os.path.join(output_dir, f"{base_name}Right.png"))

# Center
img.crop((left, top, w-right, h-bottom)).save(os.path.join(output_dir, f"{base_name}Center.png"))
