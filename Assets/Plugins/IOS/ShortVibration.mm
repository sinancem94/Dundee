#import "HapticInterface.h"
#import "ShortVibration.h"


extern "C"
{
    void Vibrate(int x)
    {
		FeedbackType type = (FeedbackType)x;
		
        switch (type) {
        case FeedbackType_Selection:
		{
            UInt32 pop = SystemSoundID(1520);
            AudioServicesPlaySystemSound(pop);
            break;
		}
        case FeedbackType_Impact_Light:
		{
            [HapticInterface generateFeedback:FeedbackType_Impact_Light];
            break;
		}
        case FeedbackType_Impact_Medium:
		{
            [HapticInterface generateFeedback:FeedbackType_Impact_Medium];
            break;
		}
        case FeedbackType_Impact_Heavy:
		{
            [HapticInterface generateFeedback:FeedbackType_Impact_Heavy];
            break;
		}
        case FeedbackType_Notification_Success:
		{
            [HapticInterface generateFeedback:FeedbackType_Notification_Success];
            break;
		}
        case FeedbackType_Notification_Warning:
		{
            [HapticInterface generateFeedback:FeedbackType_Notification_Warning];
            break;
		}
        case FeedbackType_Notification_Error:
		{
            [HapticInterface generateFeedback:FeedbackType_Notification_Error];
            break;
		}
        default:
            break;
		}
    }
}






