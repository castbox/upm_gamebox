namespace GameBox
{
    public interface IGToggle
    {
        int Id { get; set; }
        bool Value { get; set; }
        void SetValue(bool value);
    }
}