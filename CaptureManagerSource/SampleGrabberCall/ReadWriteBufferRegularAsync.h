#pragma once
#include "ReadWriteBuffer.h"

namespace CaptureManager
{
   namespace Sinks
   {
      namespace SampleGrabberCall
      {
         namespace RegularSampleGrabberCall
         {
            class ReadWriteBufferRegularAsync : public ReadWriteBuffer
            {
            public:
               ReadWriteBufferRegularAsync(DWORD aImageByteSize);

               virtual ~ReadWriteBufferRegularAsync();

               HRESULT readData(unsigned char* aPtrData, DWORD* aPtrSampleSize) override;

               void copy(const unsigned char* aPtrSource, unsigned char* aPtrDestination, DWORD aSampleSize,
                         bool aState) override;
            };
         }
      }
   }
}
