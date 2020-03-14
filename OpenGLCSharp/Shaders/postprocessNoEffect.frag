#version 330

in vec2 texCoord;

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
    vec4 color = texture(u_texture, texCoord );
    if(color.r == 0 && color.g == 0 && color.b == 0){
        gl_FragDepth = 1000000;
    } else{       
        gl_FragDepth = 0;
    }

    vec4 res = color + gl_FragColor;

    gl_FragColor = res;   
}