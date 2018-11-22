#include <stdint.h>

int playSound(
    int16_t *waveData, 
    uint32_t nData,
    uint32_t samplingRate, 
    uint16_t numChannels,
    uint32_t soundmiliSec);

void printDebugInfo();

bool checkCompatibility();
