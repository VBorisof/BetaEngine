using Beta.Common;

namespace Beta.Gui.Events;

public class GuiHandler<T> : Singleton<T>, IGuiHandler where T : new()
{
}

public interface IGuiHandler
{
}
