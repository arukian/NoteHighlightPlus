using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace NoteHighlightAddin.Highlighting.KeywordGroups.Metadata
{
    /// <summary>
    /// Guarda y carga los metadatos de los grupos utilizando
    /// un archivo JSON junto al archivo .lang correspondiente.
    /// </summary>
    public sealed class JsonLanguageGroupMetadataStore
        : ILanguageGroupMetadataStore
    {
        private const string MetadataFileSuffix =
            ".groups.json";

        public LanguageGroupMetadata Load(
            string languageFilePath)
        {
            string metadataFilePath =
                GetMetadataFilePath(
                    languageFilePath);

            if (!File.Exists(metadataFilePath))
            {
                return new LanguageGroupMetadata();
            }

            try
            {
                using (FileStream stream =
                    File.OpenRead(metadataFilePath))
                {
                    DataContractJsonSerializer serializer =
                        CreateSerializer();

                    LanguageGroupMetadata metadata =
                        serializer.ReadObject(stream)
                            as LanguageGroupMetadata;

                    return NormalizeMetadata(
                        metadata);
                }
            }
            catch (SerializationException exception)
            {
                throw new InvalidDataException(
                    "The language group metadata file contains invalid JSON.",
                    exception);
            }
            catch (IOException exception)
            {
                throw new IOException(
                    "The language group metadata file could not be read.",
                    exception);
            }
        }

        public void Save(
            string languageFilePath,
            LanguageGroupMetadata metadata)
        {
            ValidateLanguageFilePath(
                languageFilePath);

            if (metadata == null)
            {
                throw new ArgumentNullException(
                    nameof(metadata));
            }

            string metadataFilePath =
                GetMetadataFilePath(
                    languageFilePath);

            string directoryPath =
                Path.GetDirectoryName(
                    metadataFilePath);

            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(
                    directoryPath);
            }

            LanguageGroupMetadata normalizedMetadata =
                NormalizeMetadata(
                    metadata);

            using (MemoryStream memoryStream =
                new MemoryStream())
            {
                DataContractJsonSerializer serializer =
                    CreateSerializer();

                serializer.WriteObject(
                    memoryStream,
                    normalizedMetadata);

                string json =
                    Encoding.UTF8.GetString(
                        memoryStream.ToArray());

                File.WriteAllText(
                    metadataFilePath,
                    json,
                    new UTF8Encoding(false));
            }
        }

        private static DataContractJsonSerializer CreateSerializer()
        {
            return new DataContractJsonSerializer(
                typeof(LanguageGroupMetadata));
        }

        private static LanguageGroupMetadata NormalizeMetadata(
            LanguageGroupMetadata metadata)
        {
            if (metadata == null)
            {
                return new LanguageGroupMetadata();
            }

            if (metadata.Groups == null)
            {
                metadata.Groups =
                    new System.Collections.Generic.List<GroupMetadata>();
            }

            return metadata;
        }

        private static string GetMetadataFilePath(
            string languageFilePath)
        {
            ValidateLanguageFilePath(
                languageFilePath);

            string directoryPath =
                Path.GetDirectoryName(
                    languageFilePath);

            string fileNameWithoutExtension =
                Path.GetFileNameWithoutExtension(
                    languageFilePath);

            string metadataFileName =
                fileNameWithoutExtension +
                MetadataFileSuffix;

            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return metadataFileName;
            }

            return Path.Combine(
                directoryPath,
                metadataFileName);
        }

        private static void ValidateLanguageFilePath(
            string languageFilePath)
        {
            if (string.IsNullOrWhiteSpace(languageFilePath))
            {
                throw new ArgumentException(
                    "The language file path cannot be empty.",
                    nameof(languageFilePath));
            }

            string fileName =
                Path.GetFileNameWithoutExtension(
                    languageFilePath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException(
                    "The language file path is invalid.",
                    nameof(languageFilePath));
            }
        }
    }
}