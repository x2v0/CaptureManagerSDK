#pragma once

#include <mutex>
#include <memory>

#include "../Common/BaseUnknown.h"
#include "IRead.h"
#include "IWrite.h"

namespace CaptureManager
{
	namespace Sinks
	{
		namespace SampleGrabberCall
		{
			namespace RegularSampleGrabberCall
			{
				class ReadWriteBuffer : 
					public BaseUnknown <IRead, IWrite>
				{
				public:

					ReadWriteBuffer():
						mReadyToRead(false)
					{
					}

					HRESULT init(DWORD aImageByteSize)
					{
						mImageByteSize = aImageByteSize;
						
						mData.reset(new (std::nothrow) unsigned char[aImageByteSize]);

						if (!mData)
						{
							return E_OUTOFMEMORY;
						}

						memset(mData.get(), 0, aImageByteSize);

						return S_OK;
					}

					virtual ~ReadWriteBuffer(void){}
					
					virtual void writeData(
						const BYTE* aPtrSampleBuffer,
						DWORD aSampleSize)
					{
						std::lock_guard<std::mutex> llock(mMutex);

						copy(aPtrSampleBuffer, 
							mData.get(), 
							aSampleSize > mImageByteSize ? mImageByteSize : aSampleSize,
							true);
					}					
										
				protected:

					virtual void copy(
						const unsigned char* aPtrSource,
						unsigned char *aPtrDestination,
						DWORD aSampleSize,
						bool aState) = 0;

					std::mutex mMutex;

					std::unique_ptr<unsigned char[]> mData;

					DWORD mImageByteSize;

					bool mReadyToRead;

				private:

					std::atomic<ULONG> mRefCount;

				};
			}
		}
	}
}