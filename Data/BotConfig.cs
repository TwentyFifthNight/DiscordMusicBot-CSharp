using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Satescuro.Data
{
	public sealed class BotConfig
	{
		[JsonProperty("token")]
		public string Token { get; set; }

		[JsonProperty("lavalinkpassword")]
		public string LavalinkPassword { get; set; }

		[JsonProperty("lavalinkport")]
		public string LavalinkPort { get; set; }

		[JsonProperty("lavalinkaddress")]
		public string LavalinkAddress { get; set; }

		[JsonProperty("ConnectionStrings")]
		public Dictionary<string, string> DBConnectionString { get; set; }

		public static async Task<BotConfig> LoadConfig(FileInfo file)
		{
			if (file == null || !file.Exists)
				throw new ArgumentException("Specified file is not valid or does not exist.", nameof(file));

			string json;
			using var fs = file.OpenRead();
			using var sr = new StreamReader(fs, Encoding.UTF8);
			json = await sr.ReadToEndAsync();

			BotConfig config = JsonConvert.DeserializeObject<BotConfig>(json);
			ValidateConfiguration(config);

			return config;
		}

		public static void ValidateConfiguration(BotConfig config)
		{
			// validate the config
			if (config == null || config.Token == null || config.LavalinkPassword == null
				|| config.LavalinkAddress == null || config.LavalinkPort == null
				|| config.DBConnectionString == null)
				throw new ArgumentNullException(nameof(config), "Configuration data, or one of its parts, is null.");

			if (!int.TryParse(config.LavalinkPort, out int port) || port < 0)
				throw new ArgumentNullException(nameof(config), "Invalid port value.");

		}
	}
}
