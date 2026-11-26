import os, math
from PIL import Image, ImageDraw, ImageFilter, ImageFont

OUT_DIR = "Assets/Textures/UI/Procedural"
os.makedirs(OUT_DIR, exist_ok=True)

def supersample_save(draw_fn, base_w, base_h, filename, scale=4):
    w, h = base_w * scale, base_h * scale
    im = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    draw = ImageDraw.Draw(im)
    draw_fn(draw, w, h, scale)
    out = im.resize((base_w, base_h), Image.Resampling.LANCZOS)
    out.save(os.path.join(OUT_DIR, filename), "PNG")
    print(f"Generated {filename} ({base_w}x{base_h})")

# 1. Panel Card (Normal Dark Slate Glassmorphic)
def draw_panel_card(draw, w, h, s):
    pad = 3 * s
    r = 16 * s
    # Background gradient
    for y in range(pad, h - pad):
        factor = (y - pad) / max(1, h - 2 * pad)
        r_c = int(14 * (1 - factor) + 8 * factor)
        g_c = int(22 * (1 - factor) + 14 * factor)
        b_c = int(36 * (1 - factor) + 24 * factor)
        a_c = 240
        # Draw slice
        draw.line([(pad, y), (w - pad, y)], fill=(r_c, g_c, b_c, a_c))
    
    # Mask to rounded rect
    mask = Image.new("L", (w, h), 0)
    m_draw = ImageDraw.Draw(mask)
    m_draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, fill=255)
    
    # Draw border
    draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, outline=(48, 72, 108, 255), width=int(2.2 * s))
    # Top edge highlight line
    draw.line([(pad + r, pad + int(1.5*s)), (w - pad - r, pad + int(1.5*s))], fill=(90, 130, 190, 180), width=int(1.5*s))

supersample_save(draw_panel_card, 256, 96, "panel_card.png")

# 2. Panel Card Active (Golden Glowing Selected)
def draw_panel_card_active(draw, w, h, s):
    pad = 3 * s
    r = 16 * s
    for y in range(pad, h - pad):
        factor = (y - pad) / max(1, h - 2 * pad)
        r_c = int(28 * (1 - factor) + 14 * factor)
        g_c = int(38 * (1 - factor) + 20 * factor)
        b_c = int(58 * (1 - factor) + 32 * factor)
        draw.line([(pad, y), (w - pad, y)], fill=(r_c, g_c, b_c, 248))
    # Glowing golden border
    draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, outline=(255, 204, 0, 255), width=int(3.0 * s))
    draw.line([(pad + r, pad + int(1.5*s)), (w - pad - r, pad + int(1.5*s))], fill=(255, 240, 150, 240), width=int(2*s))

supersample_save(draw_panel_card_active, 256, 96, "panel_card_active.png")

# 3. Pill Capsule (Smooth for Coins, Gems, Badges)
def draw_pill(draw, w, h, s):
    pad = 3 * s
    r = (h - 2 * pad) // 2
    for y in range(pad, h - pad):
        factor = (y - pad) / max(1, h - 2 * pad)
        r_c = int(12 * (1 - factor) + 7 * factor)
        g_c = int(18 * (1 - factor) + 12 * factor)
        b_c = int(30 * (1 - factor) + 20 * factor)
        draw.line([(pad, y), (w - pad, y)], fill=(r_c, g_c, b_c, 235))
    draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, outline=(42, 65, 96, 255), width=int(2 * s))
    draw.line([(pad + r, pad + int(1.2*s)), (w - pad - r, pad + int(1.2*s))], fill=(80, 115, 170, 160), width=int(1.2*s))

supersample_save(draw_pill, 160, 56, "pill_capsule.png")

# 4. Square Icon Button (Mail, Settings, Exit)
def draw_square_btn(draw, w, h, s):
    pad = 3 * s
    r = 14 * s
    for y in range(pad, h - pad):
        factor = (y - pad) / max(1, h - 2 * pad)
        r_c = int(16 * (1 - factor) + 10 * factor)
        g_c = int(24 * (1 - factor) + 15 * factor)
        b_c = int(38 * (1 - factor) + 26 * factor)
        draw.line([(pad, y), (w - pad, y)], fill=(r_c, g_c, b_c, 240))
    draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, outline=(48, 72, 108, 255), width=int(2.2 * s))
    draw.line([(pad + r, pad + int(1.5*s)), (w - pad - r, pad + int(1.5*s))], fill=(90, 130, 190, 180), width=int(1.5*s))

supersample_save(draw_square_btn, 80, 80, "btn_icon_square.png")

# 5. Play Button (Angled Golden Parallelogram / Trapezoid)
def draw_btn_play(draw, w, h, s):
    pad = 4 * s
    slant = 32 * s
    pts = [
        (pad + slant, pad),
        (w - pad, pad),
        (w - pad - slant, h - pad),
        (pad, h - pad)
    ]
    # Draw gradient inside polygon
    mask = Image.new("L", (w, h), 0)
    m_draw = ImageDraw.Draw(mask)
    m_draw.polygon(pts, fill=255)
    
    grad = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    g_draw = ImageDraw.Draw(grad)
    for y in range(h):
        factor = y / max(1, h)
        r_c = int(255 * (1 - factor) + 245 * factor)
        g_c = int(218 * (1 - factor) + 138 * factor)
        b_c = int(26 * (1 - factor) + 0 * factor)
        g_draw.line([(0, y), (w, y)], fill=(r_c, g_c, b_c, 255))
    
    # Speed racing stripe accent in bottom left
    stripe_w = 40 * s
    for i in range(4):
        sx = pad + i * 9 * s
        g_draw.polygon([(sx + slant, pad), (sx + 4*s + slant, pad), (sx + 4*s, h - pad), (sx, h - pad)], fill=(255, 255, 255, 45))
        
    grad.putalpha(mask)
    draw.bitmap((0, 0), grad)
    draw.polygon(pts, outline=(255, 245, 160, 255), width=int(3 * s))

supersample_save(draw_btn_play, 340, 100, "btn_play_shape.png")

# 6. Modal Background Glass
def draw_modal_glass(draw, w, h, s):
    pad = 4 * s
    r = 20 * s
    for y in range(pad, h - pad):
        factor = (y - pad) / max(1, h - 2 * pad)
        r_c = int(10 * (1 - factor) + 6 * factor)
        g_c = int(15 * (1 - factor) + 9 * factor)
        b_c = int(24 * (1 - factor) + 15 * factor)
        draw.line([(pad, y), (w - pad, y)], fill=(r_c, g_c, b_c, 248))
    draw.rounded_rectangle([pad, pad, w - pad, h - pad], radius=r, outline=(55, 85, 130, 255), width=int(2.5 * s))
    draw.line([(pad + r, pad + int(1.5*s)), (w - pad - r, pad + int(1.5*s))], fill=(110, 160, 240, 200), width=int(2*s))

supersample_save(draw_modal_glass, 400, 300, "panel_modal_glass.png")

# 7. Red Notification Dot
def draw_red_badge(draw, w, h, s):
    draw.ellipse([2*s, 2*s, w - 2*s, h - 2*s], fill=(240, 30, 60, 255), outline=(255, 255, 255, 200), width=int(1.5*s))
    draw.ellipse([6*s, 4*s, 14*s, 10*s], fill=(255, 160, 180, 200))

supersample_save(draw_red_badge, 32, 32, "badge_red_dot.png")

# ==========================================
# CLEAN ICONS (64x64)
# ==========================================

# Profile / Helmet Icon
def draw_icon_profile(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Helmet dome
    draw.pieslice([cx - 20*s, cy - 22*s, cx + 20*s, cy + 18*s], 180, 360, fill=(230, 235, 245, 255))
    # Visor
    draw.rounded_rectangle([cx - 18*s, cy - 6*s, cx + 18*s, cy + 4*s], radius=3*s, fill=(30, 40, 60, 255), outline=(255, 200, 0, 255), width=int(1.5*s))
    # Chin guard
    draw.rounded_rectangle([cx - 16*s, cy + 5*s, cx + 16*s, cy + 16*s], radius=4*s, fill=(200, 210, 225, 255))
    # Neck / Shoulders
    draw.pieslice([cx - 26*s, cy + 12*s, cx + 26*s, cy + 32*s], 180, 360, fill=(60, 90, 140, 255))

supersample_save(draw_icon_profile, 64, 64, "icon_profile.png")

# Coin Icon
def draw_icon_coin(draw, w, h, s):
    cx, cy = w // 2, h // 2
    r = 24 * s
    draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(255, 195, 0, 255), outline=(220, 150, 0, 255), width=int(2.5*s))
    draw.ellipse([cx - r + 4*s, cy - r + 4*s, cx + r - 4*s, cy + r - 4*s], outline=(255, 230, 100, 255), width=int(1.8*s))
    # Dollar / Star mark
    draw.ellipse([cx - 7*s, cy - 7*s, cx + 7*s, cy + 7*s], fill=(255, 235, 130, 255))
    draw.line([(cx, cy - 10*s), (cx, cy + 10*s)], fill=(180, 120, 0, 255), width=int(2.2*s))

supersample_save(draw_icon_coin, 64, 64, "icon_coin.png")

# Gem / Diamond Icon
def draw_icon_gem(draw, w, h, s):
    cx, cy = w // 2, h // 2
    pts_outer = [
        (cx - 15*s, cy - 18*s),
        (cx + 15*s, cy - 18*s),
        (cx + 25*s, cy - 4*s),
        (cx, cy + 22*s),
        (cx - 25*s, cy - 4*s)
    ]
    draw.polygon(pts_outer, fill=(0, 200, 255, 255), outline=(200, 250, 255, 255), width=int(2*s))
    # Facet lines
    draw.line([(cx - 15*s, cy - 18*s), (cx - 8*s, cy - 4*s), (cx, cy + 22*s)], fill=(255, 255, 255, 200), width=int(1.5*s))
    draw.line([(cx + 15*s, cy - 18*s), (cx + 8*s, cy - 4*s), (cx, cy + 22*s)], fill=(10, 130, 190, 200), width=int(1.5*s))
    draw.line([(cx - 25*s, cy - 4*s), (cx + 25*s, cy - 4*s)], fill=(220, 255, 255, 220), width=int(1.5*s))

supersample_save(draw_icon_gem, 64, 64, "icon_gem.png")

# Mail Icon
def draw_icon_mail(draw, w, h, s):
    cx, cy = w // 2, h // 2
    draw.rounded_rectangle([cx - 22*s, cy - 15*s, cx + 22*s, cy + 15*s], radius=3*s, fill=(220, 230, 245, 255), outline=(100, 140, 190, 255), width=int(2*s))
    draw.line([(cx - 22*s, cy - 15*s), (cx, cy + 3*s), (cx + 22*s, cy - 15*s)], fill=(80, 110, 160, 255), width=int(2*s))

supersample_save(draw_icon_mail, 64, 64, "icon_mail.png")

# Settings / Gear Icon
def draw_icon_settings(draw, w, h, s):
    cx, cy = w // 2, h // 2
    r_out, r_in = 20 * s, 14 * s
    draw.ellipse([cx - r_out, cy - r_out, cx + r_out, cy + r_out], fill=(200, 215, 235, 255))
    # 6 cogs
    for i in range(6):
        ang = i * (math.pi / 3)
        tx = cx + int((r_out + 3*s) * math.cos(ang))
        ty = cy + int((r_out + 3*s) * math.sin(ang))
        draw.ellipse([tx - 5*s, ty - 5*s, tx + 5*s, ty + 5*s], fill=(200, 215, 235, 255))
    # Center hole
    draw.ellipse([cx - 8*s, cy - 8*s, cx + 8*s, cy + 8*s], fill=(16, 24, 38, 255), outline=(100, 140, 190, 255), width=int(1.5*s))

supersample_save(draw_icon_settings, 64, 64, "icon_settings.png")

# Events / Trophy Icon
def draw_icon_trophy(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Cup bowl
    draw.pieslice([cx - 16*s, cy - 20*s, cx + 16*s, cy + 12*s], 0, 180, fill=(255, 205, 30, 255), outline=(220, 160, 10, 255), width=int(2*s))
    draw.line([(cx - 16*s, cy - 10*s), (cx + 16*s, cy - 10*s)], fill=(255, 230, 100, 255), width=int(2*s))
    # Handles
    draw.arc([cx - 24*s, cy - 18*s, cx - 12*s, cy - 2*s], 90, 270, fill=(255, 205, 30, 255), width=int(3*s))
    draw.arc([cx + 12*s, cy - 18*s, cx + 24*s, cy - 2*s], 270, 90, fill=(255, 205, 30, 255), width=int(3*s))
    # Stem & base
    draw.line([(cx, cy + 6*s), (cx, cy + 16*s)], fill=(220, 160, 10, 255), width=int(4*s))
    draw.rounded_rectangle([cx - 12*s, cy + 16*s, cx + 12*s, cy + 22*s], radius=2*s, fill=(255, 205, 30, 255), outline=(200, 140, 0, 255), width=int(1.5*s))

supersample_save(draw_icon_trophy, 64, 64, "icon_events.png")

# Garage / Supercar Icon
def draw_icon_garage(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Sleek supercar profile
    pts = [
        (cx - 24*s, cy + 6*s),
        (cx - 22*s, cy),
        (cx - 14*s, cy - 2*s),
        (cx - 6*s, cy - 10*s),
        (cx + 8*s, cy - 10*s),
        (cx + 16*s, cy - 3*s),
        (cx + 25*s, cy + 2*s),
        (cx + 25*s, cy + 7*s),
        (cx - 24*s, cy + 7*s)
    ]
    draw.polygon(pts, fill=(225, 235, 250, 255), outline=(255, 200, 0, 255), width=int(1.5*s))
    # Windows
    draw.polygon([(cx - 4*s, cy - 8*s), (cx + 6*s, cy - 8*s), (cx + 12*s, cy - 2*s), (cx - 4*s, cy - 2*s)], fill=(30, 45, 70, 255))
    # Wheels
    draw.ellipse([cx - 18*s, cy + 3*s, cx - 8*s, cy + 13*s], fill=(30, 35, 45, 255), outline=(200, 210, 230, 255), width=int(1.8*s))
    draw.ellipse([cx + 9*s, cy + 3*s, cx + 19*s, cy + 13*s], fill=(30, 35, 45, 255), outline=(200, 210, 230, 255), width=int(1.8*s))

supersample_save(draw_icon_garage, 64, 64, "icon_garage.png")

# Customization / Wrench & Brush Icon
def draw_icon_custom(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Crossed wrench & spray gun
    draw.line([(cx - 16*s, cy - 16*s), (cx + 16*s, cy + 16*s)], fill=(220, 230, 245, 255), width=int(4*s))
    draw.ellipse([cx - 20*s, cy - 20*s, cx - 8*s, cy - 8*s], fill=(220, 230, 245, 255))
    draw.ellipse([cx - 16*s, cy - 16*s, cx - 12*s, cy - 12*s], fill=(16, 24, 38, 255))
    
    draw.line([(cx + 16*s, cy - 16*s), (cx - 16*s, cy + 16*s)], fill=(255, 180, 40, 255), width=int(3.5*s))
    draw.polygon([(cx + 10*s, cy - 20*s), (cx + 20*s, cy - 10*s), (cx + 16*s, cy - 6*s), (cx + 6*s, cy - 16*s)], fill=(255, 180, 40, 255))

supersample_save(draw_icon_custom, 64, 64, "icon_custom.png")

# Store / Shopping Cart Icon
def draw_icon_store(draw, w, h, s):
    cx, cy = w // 2, h // 2
    draw.line([(cx - 20*s, cy - 14*s), (cx - 14*s, cy - 14*s), (cx - 8*s, cy + 8*s), (cx + 16*s, cy + 8*s), (cx + 20*s, cy - 8*s), (cx - 12*s, cy - 8*s)], fill=(220, 235, 250, 255), width=int(2.5*s))
    draw.line([(cx - 6*s, cy), (cx + 18*s, cy)], fill=(120, 170, 230, 255), width=int(1.8*s))
    # Wheels
    draw.ellipse([cx - 8*s, cy + 11*s, cx - 2*s, cy + 17*s], fill=(255, 200, 30, 255))
    draw.ellipse([cx + 11*s, cy + 11*s, cx + 17*s, cy + 17*s], fill=(255, 200, 30, 255))

supersample_save(draw_icon_store, 64, 64, "icon_store.png")

# Cars Icon
def draw_icon_cars(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Dual racing cars
    # Car 1 (top back)
    draw.polygon([(cx - 18*s, cy - 4*s), (cx - 6*s, cy - 12*s), (cx + 8*s, cy - 12*s), (cx + 16*s, cy - 4*s)], fill=(140, 165, 200, 255))
    # Car 2 (front main)
    pts2 = [(cx - 20*s, cy + 8*s), (cx - 16*s, cy + 1*s), (cx - 4*s, cy - 6*s), (cx + 8*s, cy - 6*s), (cx + 18*s, cy + 1*s), (cx + 22*s, cy + 8*s)]
    draw.polygon(pts2, fill=(235, 240, 255, 255), outline=(255, 190, 0, 255), width=int(1.5*s))
    draw.ellipse([cx - 15*s, cy + 6*s, cx - 7*s, cy + 14*s], fill=(20, 25, 35, 255), outline=(200, 210, 230, 255), width=int(1.5*s))
    draw.ellipse([cx + 7*s, cy + 6*s, cx + 15*s, cy + 14*s], fill=(20, 25, 35, 255), outline=(200, 210, 230, 255), width=int(1.5*s))

supersample_save(draw_icon_cars, 64, 64, "icon_cars.png")

# Upgrades Icon (Turbo / Tools)
def draw_icon_upgrades(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # Turbo snail shell
    draw.ellipse([cx - 16*s, cy - 14*s, cx + 12*s, cy + 14*s], fill=(225, 235, 250, 255), outline=(255, 170, 0, 255), width=int(2*s))
    draw.ellipse([cx - 8*s, cy - 6*s, cx + 4*s, cy + 6*s], fill=(30, 45, 70, 255), outline=(255, 200, 0, 255), width=int(1.5*s))
    # Turbo exhaust pipe
    draw.polygon([(cx + 6*s, cy - 14*s), (cx + 20*s, cy - 14*s), (cx + 20*s, cy - 2*s), (cx + 10*s, cy - 2*s)], fill=(225, 235, 250, 255), outline=(255, 170, 0, 255), width=int(1.5*s))

supersample_save(draw_icon_upgrades, 64, 64, "icon_upgrades.png")

# Missions Icon (Target Bullseye)
def draw_icon_missions(draw, w, h, s):
    cx, cy = w // 2, h // 2
    draw.ellipse([cx - 22*s, cy - 22*s, cx + 22*s, cy + 22*s], fill=(240, 50, 60, 255))
    draw.ellipse([cx - 16*s, cy - 16*s, cx + 16*s, cy + 16*s], fill=(240, 245, 255, 255))
    draw.ellipse([cx - 10*s, cy - 10*s, cx + 10*s, cy + 10*s], fill=(240, 50, 60, 255))
    draw.ellipse([cx - 4*s, cy - 4*s, cx + 4*s, cy + 4*s], fill=(255, 255, 255, 255))

supersample_save(draw_icon_missions, 64, 64, "icon_missions.png")

# Leaderboard Icon (Podium)
def draw_icon_leaderboard(draw, w, h, s):
    cx, cy = w // 2, h // 2
    # 2nd place (left)
    draw.rounded_rectangle([cx - 22*s, cy - 4*s, cx - 8*s, cy + 18*s], radius=2*s, fill=(180, 200, 225, 255), outline=(100, 130, 170, 255), width=int(1.5*s))
    # 1st place (center, tallest)
    draw.rounded_rectangle([cx - 7*s, cy - 14*s, cx + 7*s, cy + 18*s], radius=2*s, fill=(255, 205, 30, 255), outline=(210, 150, 0, 255), width=int(1.8*s))
    # 3rd place (right)
    draw.rounded_rectangle([cx + 8*s, cy + 2*s, cx + 22*s, cy + 18*s], radius=2*s, fill=(205, 140, 80, 255), outline=(150, 90, 40, 255), width=int(1.5*s))

supersample_save(draw_icon_leaderboard, 64, 64, "icon_leaderboard.png")

# Close ✕ Icon
def draw_icon_close(draw, w, h, s):
    cx, cy = w // 2, h // 2
    draw.line([(cx - 12*s, cy - 12*s), (cx + 12*s, cy + 12*s)], fill=(255, 255, 255, 255), width=int(3.5*s))
    draw.line([(cx + 12*s, cy - 12*s), (cx - 12*s, cy + 12*s)], fill=(255, 255, 255, 255), width=int(3.5*s))

supersample_save(draw_icon_close, 48, 48, "icon_close.png")

# Location Thumbnails (160x100)
def draw_thumb_alpine(draw, w, h, s):
    # Blue sky to white snow mountain
    for y in range(h):
        factor = y / h
        draw.line([(0, y), (w, y)], fill=(int(40 + 100*factor), int(100 + 120*factor), int(180 + 75*factor), 255))
    # Mountain peaks
    draw.polygon([(0, h), (w*0.35, h*0.25), (w*0.7, h), (0, h)], fill=(235, 245, 255, 255))
    draw.polygon([(w*0.3, h), (w*0.65, h*0.18), (w, h), (w*0.3, h)], fill=(250, 255, 255, 255))
    draw.polygon([(w*0.5, h), (w*0.85, h*0.32), (w*1.1, h), (w*0.5, h)], fill=(210, 225, 245, 255))
    # Border
    draw.rectangle([0, 0, w-1, h-1], outline=(120, 180, 255, 255), width=int(2*s))

supersample_save(draw_thumb_alpine, 160, 100, "thumb_alpine.png")

def draw_thumb_desert(draw, w, h, s):
    # Sunset sky to orange dunes
    for y in range(h):
        factor = y / h
        draw.line([(0, y), (w, y)], fill=(int(240 - 20*factor), int(120 + 20*factor), int(40 - 20*factor), 255))
    # Dunes
    draw.polygon([(0, h), (w*0.4, h*0.45), (w, h*0.8), (w, h), (0, h)], fill=(225, 140, 40, 255))
    draw.polygon([(w*0.2, h), (w*0.8, h*0.55), (w, h), (w*0.2, h)], fill=(245, 170, 60, 255))
    draw.rectangle([0, 0, w-1, h-1], outline=(255, 180, 70, 255), width=int(2*s))

supersample_save(draw_thumb_desert, 160, 100, "thumb_desert.png")

def draw_thumb_city(draw, w, h, s):
    # Cyberpunk night city
    for y in range(h):
        factor = y / h
        draw.line([(0, y), (w, y)], fill=(int(15 + 20*factor), int(10 + 15*factor), int(40 + 30*factor), 255))
    # Skyscrapers
    draw.rectangle([w*0.1, h*0.35, w*0.28, h], fill=(25, 45, 80, 255), outline=(0, 240, 255, 200), width=int(1.5*s))
    draw.rectangle([w*0.32, h*0.20, w*0.52, h], fill=(45, 25, 75, 255), outline=(255, 0, 180, 200), width=int(1.5*s))
    draw.rectangle([w*0.58, h*0.30, w*0.75, h], fill=(20, 50, 75, 255), outline=(0, 240, 255, 200), width=int(1.5*s))
    draw.rectangle([w*0.80, h*0.45, w*0.95, h], fill=(40, 20, 60, 255), outline=(255, 200, 0, 200), width=int(1.5*s))
    draw.rectangle([0, 0, w-1, h-1], outline=(0, 220, 255, 255), width=int(2*s))

supersample_save(draw_thumb_city, 160, 100, "thumb_city.png")

def draw_thumb_highway(draw, w, h, s):
    # Coastal Ocean Highway with golden sunset & asphalt road
    for y in range(h):
        factor = y / h
        draw.line([(0, y), (w, y)], fill=(int(20 + 30*factor), int(120 + 50*factor), int(190 - 40*factor), 255))
    # Ocean blue
    draw.rectangle([0, int(h*0.42), w, h], fill=(20, 90, 140, 255))
    # Coastal road perspective
    draw.polygon([(w*0.2, h), (w*0.46, h*0.42), (w*0.54, h*0.42), (w*0.8, h)], fill=(45, 52, 60, 255))
    # Center white dash line
    draw.line([(w*0.5, h*0.43), (w*0.5, h)], fill=(255, 255, 255, 255), width=int(2*s))
    # Yellow shoulder
    draw.line([(w*0.23, h), (w*0.47, h*0.43)], fill=(255, 215, 0, 255), width=int(2*s))
    draw.rectangle([0, 0, w-1, h-1], outline=(255, 215, 0, 255), width=int(2.5*s))

supersample_save(draw_thumb_highway, 160, 100, "thumb_highway.png")

print("ALL PROCEDURAL UI ASSETS GENERATED SUCCESSFULLY!")
