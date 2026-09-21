namespace EduAI.Court
{
    public interface IInteractable
    {
        string GetInteractionText();
        void Interact();
    }
    public interface IFocusFeedback
    {
        void SetFocused(bool focused);
    }
}
