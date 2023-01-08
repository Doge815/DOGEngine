//modified version of https://learnopengl.com/PBR/Lighting
#version 330 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec2 coord;
out vec3 WorldPos;
out vec3 Normal;

uniform mat4 projection;
uniform mat4 view;
uniform mat4 model;

void main()
{
    coord = aTexCoord;
    Normal = mat3(model) * aNormal;   

    WorldPos = vec3( vec4(aPosition, 1.0) * model);
    gl_Position =  vec4(aPosition, 1.0) * model * view * projection;
}