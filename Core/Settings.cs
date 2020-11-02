using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;

namespace ItemVisualizer.Core
{
    public class Settings : ISettings
    {
        public ToggleNode Enable { get; set; }

        public Settings()
        {
            Enable = new ToggleNode(true);
        }

        //Whites
        public ColorNode NormalBorder { get; set; } = new ColorNode(Color.White);

        //Magics
        public ColorNode MagicBorder { get; set; } = new ColorNode(Color.SkyBlue);

        //Rares
        public ColorNode RareBorder { get; set; } = new ColorNode(Color.Yellow);

        //Uniques
        public ColorNode UniqueBorder { get; set; } = new ColorNode(Color.Gold);

        //Corrupted
        public ColorNode CorruptedBorder { get; set; } = new ColorNode(Color.Red);
    }
}