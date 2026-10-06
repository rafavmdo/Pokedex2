using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokedexApp
{
    // Classe para os sprites do Pokémon
    public class PokemonSprites
    {
        public string? front_default { get; set; }
    }

    // Classe para os tipos
    public class PokemonType
    {
        public TypeInfo type { get; set; } = new TypeInfo();
    }

    public class TypeInfo
    {
        public string name { get; set; } = "";
    }

    // Dados principais do Pokémon
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

                // Mantém o GIF com o tamanho correto
                SizeMode = PictureBoxSizeMode.Zoom,

                BorderStyle = BorderStyle.FixedSingle
            };

            labelNome = new Label
            {
                Left = 20,
                Top = 310,
                Width = 380,
                Height = 30,

                Font = new Font(
                    "Segoe UI",
                    14,
                    FontStyle.Bold
                ),

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

        private async void BotaoBuscar_Click(
            object? sender,
            EventArgs e)
        {
            int numero = (int)numeroInput.Value;

            botaoBuscar.Enabled = false;

            labelStatus.Text = "Buscando...";

            labelNome.Text = "";
            labelInfo.Text = "";

            // Remove imagem anterior
            if (pictureBox.Image != null)
            {
                pictureBox.Image.Dispose();
                pictureBox.Image = null;
            }

            try
            {
                PokemonData pokemon =
                    await BuscarPokemonAsync(numero);

                await ExibirPokemonAsync(pokemon);

                labelStatus.Text = "";
            }
            catch (Exception ex)
            {
                labelStatus.Text =
                    "Erro: " + ex.Message;
            }
            finally
            {
                botaoBuscar.Enabled = true;
            }
        }

        private async Task<PokemonData> BuscarPokemonAsync(
            int numero)
        {
            // URL da PokeAPI
            string url =
                $"https://pokeapi.co/api/v2/pokemon/{numero}";

            string json =
                await _http.GetStringAsync(url);

            var opcoes =
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

            PokemonData? pokemon =
                JsonSerializer.Deserialize<PokemonData>(
                    json,
                    opcoes
                );

            if (pokemon == null)
            {
                throw new Exception(
                    "Não foi possível ler os dados do Pokémon."
                );
            }

            return pokemon;
        }

        private async Task ExibirPokemonAsync(
            PokemonData pokemon)
        {
            // ==========================================
            // NOME
            // ==========================================

            string nomeFormatado =
                char.ToUpper(pokemon.name[0])
                + pokemon.name.Substring(1);

            labelNome.Text =
                $"#{pokemon.id} - {nomeFormatado}";


            // ==========================================
            // TIPOS
            // ==========================================

            string tipos =
                string.Join(
                    ", ",
                    Array.ConvertAll(
                        pokemon.types,
                        t => t.type.name
                    )
                );

            labelInfo.Text =
                $"Tipo(s): {tipos}\n" +
                $"Altura: {pokemon.height / 10.0} m   |   " +
                $"Peso: {pokemon.weight / 10.0} kg";


            // ==========================================
            // GIF ANIMADO
            // ==========================================

            string urlImagem =
                $"https://raw.githubusercontent.com/" +
                $"PokeAPI/sprites/master/sprites/pokemon/" +
                $"other/showdown/{pokemon.id}.gif";


            try
            {
                labelStatus.Text =
                    "Carregando imagem...";


                // Baixa o GIF
                byte[] bytes =
                    await _http.GetByteArrayAsync(
                        urlImagem
                    );


                // Cria um arquivo temporário
                string caminhoGif =
                    Path.Combine(
                        Path.GetTempPath(),
                        $"pokemon_{pokemon.id}.gif"
                    );


                // Salva o GIF
                await File.WriteAllBytesAsync(
                    caminhoGif,
                    bytes
                );


                // Remove imagem anterior
                if (pictureBox.Image != null)
                {
                    pictureBox.Image.Dispose();
                    pictureBox.Image = null;
                }


                // Carrega o GIF
                Image imagem =
                    Image.FromFile(caminhoGif);


                // Coloca no PictureBox
                pictureBox.Image = imagem;


                // Força o PictureBox a atualizar
                pictureBox.Invalidate();


                labelStatus.Text = "";
            }
            catch (Exception ex)
            {
                labelStatus.Text =
                    "Erro ao carregar GIF: " +
                    ex.Message;
            }
        }
    }


    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.Run(
                new MainForm()
            );
        }
    }
}