
public interface IGeneticsSystem
{
    Genome Genome { get; }
    float GetTraitValue(string traitName, float defaultValue = 0f);
    void SetTraitValue(string traitName, float value);
    Genome CombineWith(IGeneticsSystem other);
}