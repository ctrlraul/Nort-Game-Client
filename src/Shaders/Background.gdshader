shader_type canvas_item;

uniform vec2 zoom = vec2(1);
uniform vec2 position = vec2(0);
uniform vec2 aspect = vec2(1);
uniform sampler2D noise;

vec4 texturePointSmooth(sampler2D tex, vec2 uv) {
	vec2 size = vec2(textureSize(tex, 0));
	vec2 pixel = vec2(1.0) / size;
	uv -= pixel * vec2(0.5);
	vec2 uv_pixels = uv * size;
	vec2 delta_pixel = fract(uv_pixels) - vec2(0.5);
	vec2 ddxy = fwidth(uv_pixels);
	vec2 mip = log2(ddxy) - 0.5;
	return textureLod(tex, uv + (clamp(delta_pixel / ddxy, 0.0, 1.0) - delta_pixel) * pixel, min(mip.x, mip.y));
}

void fragment() {
	//vec2 zoom = vec2(1.5 + sin(TIME / 2.0));
	//vec2 position = vec2(cos(TIME / 5.0), sin(TIME / 5.0));
	//vec2 aspect = vec2(1, 1080 / 1920);
	vec2 transformed_uv = (UV - 0.5) / zoom * aspect + position / zoom * aspect + 0.5;
	COLOR *= texturePointSmooth(noise, fract(transformed_uv));
}