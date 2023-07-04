shader_type canvas_item;

void fragment() {

	float brightness = texture(TEXTURE, UV).r;

	if (brightness > 0.5) {
		COLOR.a = min(COLOR.a, pow(brightness, 3));
	} else {
		COLOR.a = 0.0;
	}

}
