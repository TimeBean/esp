#pragma once
#include <WString.h>

class Ihttp_service
{
public:
    virtual void init() = 0;
    virtual String handle() = 0;
};