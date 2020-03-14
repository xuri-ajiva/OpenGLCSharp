#version 330

in vec2 v_tex_coords;

uniform sampler2D u_texture;

uniform highp float frequency = 8;
uniform highp float amplitude = 0.02;
uniform highp float time;

in vec3 v_position;


uniform float rt_w = 800; // screen
uniform float rt_h = 800; // screen  
uniform float vx_offset = 1;
uniform float pixel_w = 1.0f; // 15.0
uniform float pixel_h = 1.0f; // 10.0

void main() {
    highp vec2 pulse = sin(time - frequency * v_tex_coords);
    highp vec2 coord = v_tex_coords + amplitude * vec2(pulse.x, -pulse.x);
    //gl_FragColor = texture2D(source, coord) * qt_Opacity;
                
    //float average = (color.r ,color.g +  color.b) / 3.0f;
                                                                
    //gl_FragColor =  color;// vec4(average , average , average, color.a);

    

    vec3 tc = vec3(1.0, 0.0, 0.0);
    if (coord.x < (vx_offset-0.005)) {
        float dx = pixel_w * (1./rt_w);
        float dy = pixel_h * (1./rt_h);
        coord =  vec2(dx*floor(coord.x/dx), dy*floor(coord.y/dy)) ;
    }
    else if (v_tex_coords.x>=(vx_offset+0.005))
    {
    }
    

    vec4 color = texture(u_texture, coord );

    gl_FragColor = color;   
}