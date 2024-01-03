shader_type canvas_item;

const vec3 white = vec3(1);
const float speed = 0.05;
const float dissolve_scale = 0.1;

uniform float dissolve_threshold = 1;
uniform float spawn_threshold = 1;
uniform bool shiny = true;
//uniform vec4 color: source_color = vec4(0, 1, 0.5, 1);
//uniform vec4 color_shiny: source_color = vec4(0, 1, 0.6, 1);
uniform sampler2D shiny_reflections_texture;
uniform sampler2D dissolve_threshold_texture;

void fragment() {

	if (shiny) {

		float aspect = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
		float brightness = texture(TEXTURE, UV).r;
		vec2 reflections_uv = fract(vec2(SCREEN_UV.x - SCREEN_UV.y * aspect) * 0.2 + vec2(TIME * speed));

		//COLOR *= color_shiny;
		COLOR.rgb += (sin(TIME) + 1.0) * 0.025;

		if (texture(shiny_reflections_texture, reflections_uv).r > 0.0) {
			if (brightness > 0.7) {
				COLOR.rgb = white;
			} else {
				COLOR.rgb *= 2.0;
			}
		}

	}
	//else {
		//COLOR *= color;
	//}

	if (dissolve_threshold < 1.0) {

		vec2 size = vec2(textureSize(TEXTURE, 0)) * 0.5;
		float aspect = size.x / size.y;
		float dissolve_level = texture(dissolve_threshold_texture, fract(vec2(UV.x * aspect, UV.y) * dissolve_scale + size)).r;

		if (dissolve_level > dissolve_threshold) {
			COLOR.a = 0.0;
		}

	} else if (spawn_threshold < 1.0) {

		vec2 size = vec2(textureSize(TEXTURE, 0)) * 0.5;
		float aspect = size.x / size.y;
		float dissolve_level = texture(dissolve_threshold_texture, fract(vec2(UV.x * aspect, UV.y) * dissolve_scale + size)).r;

		if (dissolve_level > spawn_threshold) {
			COLOR.rgb = vec3(1);
		}

	}

}
