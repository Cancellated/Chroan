public class Rule
{
    //一个rule是一句话，但是目前其实只需要检测是否是一句话，rule的种类目前只与名词有关
    //关于规则是否生效的检测，此处的IsActive是被动改变，被IS这个WordObject改变（待拓展）
    public ObjectType Noun { get; set; } // 名词
    public ObjectType Verb { get; set; } // 动词
    public ObjectType Adjective { get; set; } // 形容词
    public bool IsActive { get; set; } // 规则是否生效

    // 构造函数示例
    public Rule(ObjectType noun, ObjectType verb, ObjectType adjective)
    {
        Noun = noun;
        Verb = verb;
        Adjective = adjective;
        IsActive = true;
    }
}