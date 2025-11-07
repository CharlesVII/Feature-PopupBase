using Cysharp.Threading.Tasks;

namespace Features.Popups
{
    public interface IAnimationPhase
    {
        void PreAnimation();
        UniTask PlayAsync();
        void AfterAnimation();
    }
}