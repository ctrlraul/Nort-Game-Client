shader_type canvas_item;

uniform float progress = 0.5;

void fragment() {
	if (UV.r > 0.5) {
		if (UV.r - 0.5 > progress * 0.5) {
			COLOR = vec4(0);
		}
	} else {
		if (0.5 - UV.r > progress * 0.5) {
			COLOR = vec4(0);
		}
	}
}
