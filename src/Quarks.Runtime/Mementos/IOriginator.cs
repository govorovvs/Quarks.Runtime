namespace Quarks.Runtime.Mementos
{
    /// <summary>
    /// Some object that has an internal state.
    /// </summary>
    /// <typeparam name="TMemento"></typeparam>
    public interface IOriginator<TMemento> where TMemento : IMemento
    {
        TMemento GetState();

        void SetState(TMemento state);
    }
}