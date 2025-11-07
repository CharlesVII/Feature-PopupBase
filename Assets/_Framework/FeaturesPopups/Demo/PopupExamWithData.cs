using UnityEngine;

namespace Features.Popups.Demo
{
    public class popupExamData
    {
        public string message = "This is a popup with data!";

        public popupExamData(string message)
        {
            this.message = message;
        }
    }

    public class PopupExamWithData : PopupBase<popupExamData>
    {

    }
}
