#version 330

out vec4 outputColor;
 
in vec2 v_texCoord;

uniform sampler2D texture0;
uniform sampler2D texture1;

void main()
{
    outputColor = mix(texture(texture0, v_texCoord), texture(texture1, v_texCoord), 0.2);
}