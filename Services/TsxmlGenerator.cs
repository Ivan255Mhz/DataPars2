namespace DataPars.Services;

using System.Text;

public static class TsxmlGenerator
{
    /// <summary>
    /// Создаёт TSXML для одного канала.
    /// </summary>
    public static void Create(
        string path,
        string dataFileName,
        DateTime startTime,
        int dataLength,
        string channelName,
        string units,
        string comment,
        int frequency = 2046,
        int? durationSeconds = null,
        string sensorScale = "5148,02001953125",
        string sensorSensitivity = "10,1940002441406")
    {
        int toSeconds = durationSeconds ?? (dataLength / 4) / frequency;
        if ((dataLength / 4) % frequency != 0) toSeconds++;

        var xml = $@"<?xml version=""1.0"" encoding=""windows-1251"" standalone=""no""?>
<MainRoot>
  <Culture>ru-ru</Culture>
  <DateTime>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</DateTime>
  <StartDateTime>{startTime:dd.MM.yyyy HH:mm:ss}</StartDateTime>
  <From Units=""s"">0</From>
  <To Units=""s"">{toSeconds}</To>
  <ChannelsQuantity>1</ChannelsQuantity>
  <TachoChannelsQuantity>0</TachoChannelsQuantity>
  <FrequencyPerChannel>{frequency}</FrequencyPerChannel>
  <DataFileName>.\{dataFileName}</DataFileName>
  <Channels>
    <Channel Index=""0"">
      <ChannelName>{channelName}</ChannelName>
      <DataFileName>.\{dataFileName}</DataFileName>
      <Comment>{comment}</Comment>
      <ChannelNumber>1</ChannelNumber>
      <ChannelFrequency>{frequency}</ChannelFrequency>
      <DataOffset>0</DataOffset>
      <DataLength>{dataLength}</DataLength>
      <Units>{units}</Units>
      <RealDataType>Single</RealDataType>
      <DataType>Single</DataType>
      <Scale>mV</Scale>
      <SensorScale>{sensorScale}</SensorScale>
      <SensorSensitivity>{sensorSensitivity}</SensorSensitivity>
      <GainFactor>1</GainFactor>
    </Channel>
  </Channels>
</MainRoot>";

        // Явно кодируем строку через windows-1251 байты
        var enc = Encoding.GetEncoding("windows-1251");
        var bytes = enc.GetBytes(xml);
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// Создаёт общий TSXML для нескольких каналов эскалатора.
    /// Каждый канал ссылается на свой отдельный .bin файл.
    /// </summary>
    public static void CreateMultiChannel(
        string path,
        DateTime overallStartTime,
        List<ChannelInfo> channels,
        int frequency = 2046)
    {
        // Длительность = максимум по всем каналам
        int maxDuration = channels.Max(ch =>
        {
            int sec = (ch.DataLength / 4) / frequency;
            if ((ch.DataLength / 4) % frequency != 0) sec++;
            return sec;
        });

        var encoding = Encoding.GetEncoding("windows-1251");
        var settings = new System.Xml.XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            Encoding = encoding
        };

        using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
        using (var writer = System.Xml.XmlWriter.Create(fileStream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("MainRoot");

            writer.WriteElementString("Culture", "ru-ru");
            writer.WriteElementString("DateTime", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            writer.WriteElementString("StartDateTime", overallStartTime.ToString("dd.MM.yyyy HH:mm:ss"));

            writer.WriteStartElement("From");
            writer.WriteAttributeString("Units", "s");
            writer.WriteString("0");
            writer.WriteEndElement();

            writer.WriteStartElement("To");
            writer.WriteAttributeString("Units", "s");
            writer.WriteString(maxDuration.ToString());
            writer.WriteEndElement();

            writer.WriteElementString("ChannelsQuantity", channels.Count.ToString());
            writer.WriteElementString("TachoChannelsQuantity", "0");
            writer.WriteElementString("FrequencyPerChannel", frequency.ToString());

            writer.WriteStartElement("Channels");

            for (int i = 0; i < channels.Count; i++)
            {
                var ch = channels[i];

                writer.WriteStartElement("Channel");
                writer.WriteAttributeString("Index", i.ToString());

                writer.WriteElementString("ChannelName", ch.ChannelName);
                writer.WriteElementString("DataFileName", $".\\{ch.DataFileName}");
                writer.WriteElementString("Comment", ch.Comment);
                writer.WriteElementString("ChannelNumber", (i + 1).ToString());
                writer.WriteElementString("ChannelFrequency", frequency.ToString());
                writer.WriteElementString("DataOffset", "0");
                writer.WriteElementString("DataLength", ch.DataLength.ToString());
                writer.WriteElementString("Units", ch.Units);
                writer.WriteElementString("RealDataType", "Single");
                writer.WriteElementString("DataType", "Single");
                writer.WriteElementString("Scale", "mV");
                writer.WriteElementString("SensorScale", "5148,02001953125");
                writer.WriteElementString("SensorSensitivity", "10,1940002441406");
                writer.WriteElementString("GainFactor", "1");

                writer.WriteEndElement(); // Channel
            }

            writer.WriteEndElement(); // Channels
            writer.WriteEndElement(); // MainRoot
            writer.WriteEndDocument();
        }

        // файл уже записан через fileStream
    }


    /// <summary>
    /// Создаёт TSXML для канала режима работы (формат Byte/Count — как тахометр).
    /// </summary>
    /// <param name="count">Количество записей (не байт)</param>
    public static void CreateModeChannel(
        string path,
        string dataFileName,
        DateTime startTime,
        int count,
        string channelName,
        int frequency = 2046)
    {
        // Single формат: float, 4 байта на запись, значения 0/1/2/3
        // SensorScale=1, SensorSensitivity=1 — без масштабирования
        int dataLength = count * 4;
        int toSeconds = count / frequency;
        if (count % frequency != 0) toSeconds++;

        var xml = $@"<?xml version=""1.0"" encoding=""windows-1251"" standalone=""no""?>
<MainRoot>
  <Culture>ru-ru</Culture>
  <DateTime>{DateTime.Now:dd.MM.yyyy HH:mm:ss}</DateTime>
  <StartDateTime>{startTime:dd.MM.yyyy HH:mm:ss}</StartDateTime>
  <From Units=""s"">0</From>
  <To Units=""s"">{toSeconds}</To>
  <ChannelsQuantity>1</ChannelsQuantity>
  <TachoChannelsQuantity>0</TachoChannelsQuantity>
  <FrequencyPerChannel>{frequency}</FrequencyPerChannel>
  <DataFileName>.\{dataFileName}</DataFileName>
  <Channels>
    <Channel Index=""0"">
      <ChannelName>{channelName}</ChannelName>
      <DataFileName>.\{dataFileName}</DataFileName>
      <Comment>&#x0420;&#x0435;&#x0436;&#x0438;&#x043C; &#x0440;&#x0430;&#x0431;&#x043E;&#x0442;&#x044B;: 0=&#x041D;&#x0435;&#x0442; &#x0434;&#x0430;&#x043D;&#x043D;&#x044B;&#x0445; 1=&#x0412;&#x044B;&#x043A;&#x043B;&#x044E;&#x0447;&#x0435;&#x043D; 2=&#x041F;&#x043E;&#x0434;&#x044A;&#x0451;&#x043C; 3=&#x0421;&#x043F;&#x0443;&#x0441;&#x043A;</Comment>
      <ChannelNumber>1</ChannelNumber>
      <ChannelFrequency>{frequency}</ChannelFrequency>
      <DataOffset>0</DataOffset>
      <DataLength>{dataLength}</DataLength>
      <Units>&#x0420;&#x0435;&#x0436;&#x0438;&#x043C;</Units>
      <RealDataType>Single</RealDataType>
      <DataType>Single</DataType>
      <Scale>mV</Scale>
      <SensorScale>1</SensorScale>
      <SensorSensitivity>1</SensorSensitivity>
      <GainFactor>1</GainFactor>
    </Channel>
  </Channels>
</MainRoot>";

        var enc2 = Encoding.GetEncoding("windows-1251");
        var bytes2 = enc2.GetBytes(xml);
        File.WriteAllBytes(path, bytes2);
    }
}

/// <summary>
/// Описание одного канала для многоканального tsxml.
/// </summary>
public class ChannelInfo
{
    public string   ChannelName  { get; set; } = "";
    public string   DataFileName { get; set; } = "";  // только имя файла, без пути
    public string   Comment      { get; set; } = "";
    public int      DataLength   { get; set; }
    public string   Units        { get; set; } = "";
    public DateTime StartTime    { get; set; }        // для вычисления общего MIN
}
