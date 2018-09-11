using System;
using System.Collections.Generic;

namespace NsLib.Config.Table 
{
  [ConfigConvert("test", typeof(Dictionary<int, test>), "test_Binary")]
  public class test
  {
    //物品ID
    [ConfigId(0)]
    public int id
    {get; set;}

    //物品名
    [ConfigId(1)]
    public string itemName
    {get; set;}

    //物品描述
    [ConfigId(2)]
    public string itemDesc
    {get; set;}

    //物品最大堆叠
    [ConfigId(3)]
    public int maxFlod
    {get; set;}

  }
}