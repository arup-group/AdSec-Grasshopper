
namespace AdSecCore.Builders {
  public interface IBuilder<out T> {
    T Build();
  }
}
