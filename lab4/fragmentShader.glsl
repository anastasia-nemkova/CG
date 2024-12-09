#version 330 core
in vec3 fragColor;
in vec3 fragPos;

uniform vec3 lightPos;
uniform vec3 lightDir;
uniform float cutoffAngle; // in radians
uniform vec3 lightColor;

out vec4 color;

void main()
{
    vec3 toLight = normalize(lightPos - fragPos);
    float theta = dot(toLight, normalize(-lightDir));

    if (theta > cutoffAngle)
    {
        float intensity = (theta - cutoffAngle) / (1.0 - cutoffAngle);
        vec3 resultColor = fragColor * lightColor * intensity;
        color = vec4(resultColor, 1.0);
    }
    else
    {
        color = vec4(0.0, 0.0, 0.0, 1.0);
    }
}