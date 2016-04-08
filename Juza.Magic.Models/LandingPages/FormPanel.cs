namespace Juza.Magic.Models.LandingPages
{
    public class FormPanel : LandingPagePanel
    {
        public virtual string RegistrationCode { get; set; }

        public FormPanel() {
            Type = PanelType.Form;
        }
    }
}