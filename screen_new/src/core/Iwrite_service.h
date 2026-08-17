#include <WString.h>

class Iwrite_service
{
public:
    virtual ~Iwrite_service() = default;

    virtual void print_text(const char *text, unsigned int max_length = 0) = 0;
    virtual void clear() = 0;

    void print(const String &text, unsigned int max_length = 0)
    {
        print_text(text.c_str(), max_length);
    }

    virtual void print_in_corner_of_4(const unsigned int corner_id, const float value, const char *metric) = 0;
};
