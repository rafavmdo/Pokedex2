using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace PokedexApp
{
    // Classes auxiliares para desserializar o JSON retornado pela PokeAPI
    public class PokemonSprites
    {
        public string? front_default { get; set; }
    }

    public class PokemonType
    {
        public TypeInfo type { get; set; } = new TypeInfo();
    }

    public class TypeInfo
    {
        public string name { get; set; } = "";
    }

    public class PokemonData
    {
        public string name { get; set; } = "";
        public int id { get; set; }
        public int height { get; set; }
        public int weight { get; set; }
        public PokemonSprites sprites { get; set; } = new PokemonSprites();
        public PokemonType[] types { get; set; } = Array.Empty<PokemonType>();
    }

    public class MainForm : Form
    {
        private readonly HttpClient _http = new HttpClient();

        private NumericUpDown numeroInput = null!;
        private Button botaoBuscar = null!;
        private PictureBox pictureBox = null!;
        private Label labelNome = null!;
        private Label labelInfo = null!;
        private Label labelStatus = null!;

        public MainForm()
        {
            ConfigurarJanela();
            ConfigurarControles();
        }

        private void ConfigurarJanela()
        {
            Text = "Pokédex - Clique no número para ver o Pokémon";
            Width = 420;
            Height = 520;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);
        }

        private void ConfigurarControles()
        {
            var labelTitulo = new Label
            {
                Text = "Digite o número do Pokémon (1 a 1025):",
                Left = 20,
                Top = 20,
                Width = 380
            };

            numeroInput = new NumericUpDown
            {
                Left = 20,
                Top = 50,
                Width = 100,
                Minimum = 1,
                Maximum = 1025,
                Value = 1
            };

            botaoBuscar = new Button
            {
                Text = "Buscar Pokémon",
                Left = 140,
                Top = 48,
                Width = 150,
                Height = 30
            };
            botaoBuscar.Click += BotaoBuscar_Click;

            pictureBox = new PictureBox
            {
                Left = 110,
                Top = 100,
                Width = 200,
                Height = 200,
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle
            };

            labelNome = new Label
            {
                Left = 20,
                Top = 310,
                Width = 380,
                Height = 30,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            labelInfo = new Label
            {
                Left = 20,
                Top = 350,
                Width = 380,
                Height = 80,
                TextAlign = ContentAlignment.TopCenter
            };

            labelStatus = new Label
            {
                Left = 20,
                Top = 440,
                Width = 380,
                Height = 30,
                ForeColor = Color.Gray,
                TextAlign = ContentAlignment.MiddleCenter
            };

            Controls.Add(labelTitulo);
            Controls.Add(numeroInput);
            Controls.Add(botaoBuscar);
            Controls.Add(pictureBox);
            Controls.Add(labelNome);
            Controls.Add(labelInfo);
            Controls.Add(labelStatus);
        }

        private async void BotaoBuscar_Click(object? sender, EventArgs e)
        {
            int numero = (int)numeroInput.Value;
            botaoBuscar.Enabled = false;
            labelStatus.Text = "Buscando...";
            labelNome.Text = "";
            labelInfo.Text = "";
            pictureBox.Image = null;

            try
            {
                var pokemon = await BuscarPokemonAsync(numero);
                await ExibirPokemonAsync(pokemon);
                labelStatus.Text = "";
            }
            catch (Exception ex)
            {
                labelStatus.Text = "Erro: " + ex.Message;
            }
            finally
            {
                botaoBuscar.Enabled = true;
            }
        }

        private async Task<PokemonData> BuscarPokemonAsync(int numero)
        {
            string url = $"https://pokeapi.co/api/v2/pokemon/{numero}";
            string json = await _http.GetStringAsync(url);

            var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var pokemon = JsonSerializer.Deserialize<PokemonData>(json, opcoes);

            if (pokemon == null)
                throw new Exception("Não foi possível ler os dados do Pokémon.");

            return pokemon;
        }

        private async Task ExibirPokemonAsync(PokemonData pokemon)
        {
            // Nome (primeira letra maiúscula) e número
            string nomeFormatado = char.ToUpper(pokemon.name[0]) + pokemon.name.Substring(1);
            labelNome.Text = $"#{pokemon.id} - {nomeFormatado}";

            // Tipos
            string tipos = string.Join(", ", Array.ConvertAll(pokemon.types, t => t.type.name));

            labelInfo.Text =
                $"Tipo(s): {tipos}\n" +
                $"Altura: {pokemon.height / 10.0} m   |   Peso: {pokemon.weight / 10.0} kg";

            // Imagem
            if (!string.IsNullOrEmpty(pokemon.sprites.front_default))
            {
                byte[] bytes = await _http.GetByteArrayAsync(pokemon.sprites.front_default);
                using var ms = new System.IO.MemoryStream(bytes);
                pictureBox.Image = Image.FromStream(ms);
            }
        }
    }

    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
