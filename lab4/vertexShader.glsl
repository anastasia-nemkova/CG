#version 330 core
layout(location = 0) in vec3 aPosition;
layout(location = 1) in vec3 aColor;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

out vec3 fragColor;
out vec3 fragPos;

void main()
{
    fragColor = aColor;
    fragPos = vec3(model * vec4(aPosition, 1.0));
    gl_Position = projection * view * model * vec4(aPosition, 1.0);
}