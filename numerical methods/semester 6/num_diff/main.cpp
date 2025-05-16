#include <iostream>
#include <iomanip>
#include <cmath>

using namespace std;

float func(float x)
{
    return sin(x);
}

float derivative(float x, float h)
{
    return (func(x + h) - func(x - h)) / (2 * h);
}

int main()
{
    float h = 2;
    float x = 1;

    cout << fixed << setprecision(15);
    cout << "h\t\t\tResidual\n";
    cout << "---------------------------------\n";

    for (int i = 0; i < 20; i++) {
        float residual = fabs(derivative(x, h) - cos(x));
        cout << h << "\t" << residual << endl;
        h /= 2;
    }

    return 0;
}
