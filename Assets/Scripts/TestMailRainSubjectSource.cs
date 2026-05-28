using UnityEngine;

public class TestMailRainSubjectSource : MonoBehaviour
{
    public string[] subjectPool =
    {
        "URGENT: Action Required Today",
        "Meeting moved to 9:00 AM",
        "Reminder: policy update",
        "Your inbox needs attention",
        "Budget review: final comments",
        "Canvas announcement posted",
        "Project deadline changed",
        "Follow up before COB",
        "Security alert: sign-in detected",
        "Weekly report overdue",
        "Please respond ASAP",
        "New task assigned to you"
    };

    void Start()
    {
        TestMailPopupUI popupUI = TestMailPopupUI.Instance;
        if (popupUI == null || subjectPool == null || subjectPool.Length == 0)
        {
            return;
        }

        popupUI.ShowSubject(subjectPool[Random.Range(0, subjectPool.Length)]);
    }
}
