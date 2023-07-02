shader_type canvas_item;

uniform float threshould = 0.5;

void fragment() {

	if (texture(TEXTURE, UV).r > threshould) {
		COLOR.a = 0.0;
	} else {
		COLOR.rgb *= 0.0;
	}

}
