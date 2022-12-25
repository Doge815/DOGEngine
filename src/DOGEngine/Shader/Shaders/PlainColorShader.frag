#version 330 core

uniform vec3 aColor;

in vec2 coord;

out vec4 FragColor;

void main()
{
    FragColor = vec4(aColor, 1.0);
}
