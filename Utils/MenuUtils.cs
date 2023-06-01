using WECCL.Patches;

namespace WECCL.Utils;

public static class MenuUtils
{
    public static void FindBestFit(int size, int minX, int minY, int maxX, int maxY, out int rows, out int columns, out float scale, out int startX,
        out int startY)
    {
        if (MenuPatch._optimalLayouts.TryGetValue(size, out Tuple<int, int, float, int, int> tuple))
        {
            rows = tuple.Item1;
            columns = tuple.Item2;
            scale = tuple.Item3;
            startX = tuple.Item4;
            startY = tuple.Item5;
            return;
        }

        int itemWidth = 210;
        int itemHeight = 50;
        int totalWidth = maxX - minX;
        int totalHeight = maxY - minY;
        float curScale = 1f;
        while (true)
        {
            int scaledTotalWidth = totalWidth + (int)(itemWidth * curScale);
            int scaledTotalHeight = totalHeight + (int)(itemHeight * curScale);
            int curWidth = (int)(itemWidth * curScale);
            int curHeight = (int)(itemHeight * curScale);
            int curColumns = scaledTotalWidth / curWidth;
            int curRows = scaledTotalHeight / curHeight;
            int curItems = curColumns * curRows;
            if (curItems >= size)
            {
                rows = curRows;
                columns = curColumns;
                scale = curScale;
                int curTotalWidth = curColumns * curWidth;
                startX = minX + ((scaledTotalWidth - curTotalWidth) / 2);
                int curTotalHeight = curRows * curHeight;
                startY = maxY - ((scaledTotalHeight - curTotalHeight) / 2);
                Plugin.Log.LogDebug(
                    $"Found best fit for {size} items: {rows} rows, {columns} columns, {scale} scale, {startX} startX, {startY} startY");
                MenuPatch._optimalLayouts.Add(size, new Tuple<int, int, float, int, int>(rows, columns, scale, startX, startY));
                return;
            }

            curScale /= 1.05f;
        }
    }
}