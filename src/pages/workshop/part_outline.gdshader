shader_type canvas_item;

void fragment() {

	if (COLOR.r > 0.5) {
		COLOR.a = min(COLOR.a, pow(COLOR.r, 3));
	} else {
		COLOR.a = 0.0;
	}

	COLOR.rgb = vec3(1, 1, 1);

}
