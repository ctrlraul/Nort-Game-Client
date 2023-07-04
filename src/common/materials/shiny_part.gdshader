shader_type canvas_item;

uniform sampler2D noise_texture;


void fragment(){

	float seed = (SCREEN_UV.x - SCREEN_UV.y) * 0.05 + TIME * 0.02;
	float noise = texture(noise_texture, fract(vec2(seed))).r;

  vec4 tex = texture(TEXTURE, UV);

	if (noise > 0.875) {
		if (tex.r > 0.5) {
	  	COLOR.rgb = vec3(1);
		} else {
	  	COLOR.rgb += vec3(0.3);
		}
	}

	COLOR.rgb += vec3(0.1, 0.1, 0.2) * tex.r;

	if (tex.r > 0.3) {
		COLOR.rgb += (0.3 - fract(TIME * 0.5) * 0.3) * tex.r;
	}

}
