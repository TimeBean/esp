#include <WString.h>

class Iwrite_service
{
public:
    virtual void print(const char *text) = 0;

    virtual void print(const String &text)
    {
        print(text.c_str());
    }
};