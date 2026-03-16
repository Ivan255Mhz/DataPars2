namespace DataPars.Services;

using System.Text;

public static class TsxmlGenerator
{
    /// <summary>
    /// Создает TSXML файл в формате, соответствующем эталону
    /// </summary>
    /// <param name="path">Полный путь для сохранения файла</param>
    /// <param name="dataFileName">Имя бинарного файла (с расширением .bin)</param>
    /// <param name="startTime">Время начала записи</param>
    /// <param name="dataLength">Длина данных в байтах</param>
    /// <param name="channelName">Имя канала</param>
    /// <param name="units">Единицы измерения (с HTML-мнемоникой)</param>
    /// <param name="comment">Комментарий (с HTML-мнемоникой для русских букв)</param>
    /// <param name="frequency">Частота дискретизации в Гц</param>
    /// <param name="durationSeconds">Длительность в секундах (если не указана, вычисляется из dataLength/frequency)</param>
    public static void Create(
        string path,
        string dataFileName,
        DateTime startTime,
        int dataLength,
        string channelName,
        string units,
        string comment,
        int frequency = 2046,
        int? durationSeconds = null)
    {
        // Вычисляем длительность, если не указана
        int toSeconds = durationSeconds ?? (dataLength / 4) / frequency;
        if ((dataLength / 4) % frequency != 0) toSeconds++;

        // Формируем XML в точности как в эталоне
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
      <SensorScale>5148,02001953125</SensorScale>
      <SensorSensitivity>10,1940002441406</SensorSensitivity>
      <GainFactor>1</GainFactor>
    </Channel>
  </Channels>
</MainRoot>";


        // Сохраняем файл с правильной кодировкой
        File.WriteAllText(path, xml, Encoding.GetEncoding("windows-1251"));
    }

    /// <summary>
    /// Создает TSXML файл для нескольких каналов
    /// </summary>
    public static void CreateMultiChannel(
        string path,
        string baseDataFileName,
        DateTime startTime,
        List<ChannelInfo> channels,
        int frequency = 2)
    {
        // Находим максимальную длительность
        int maxDuration = 0;
        foreach (var ch in channels)
        {
            int sec = (ch.DataLength / 4) / frequency;
            if ((ch.DataLength / 4) % frequency != 0) sec++;
            if (sec > maxDuration) maxDuration = sec;
        }

        // Формируем XML для нескольких каналов
        var sw = new StringWriter();
        var settings = new System.Xml.XmlWriterSettings { Indent = true, IndentChars = "  " };

        using (var writer = System.Xml.XmlWriter.Create(sw, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("MainRoot");

            writer.WriteElementString("Culture", "ru-ru");
            writer.WriteElementString("DateTime", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            writer.WriteElementString("StartDateTime", startTime.ToString("dd.MM.yyyy HH:mm:ss"));

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
            writer.WriteElementString("DataFileName", $".\\{baseDataFileName}");

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

                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // Channels
            writer.WriteEndElement(); // MainRoot
            writer.WriteEndDocument();
        }

        File.WriteAllText(path, sw.ToString(), Encoding.GetEncoding("windows-1251"));
    }
}

/// <summary>
/// Информация о канале для многоканального экспорта
/// </summary>
public class ChannelInfo
{
    public string ChannelName { get; set; }
    public string DataFileName { get; set; }
    public string Comment { get; set; }
    public int DataLength { get; set; }
    public string Units { get; set; }
}