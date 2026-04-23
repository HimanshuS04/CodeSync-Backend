namespace CodeSync.CollabService.OT
{
    public class OTService
    {
        // Transform op1 against op2
        // Returns transformed op1
        public OTOperation Transform(
            OTOperation op1, OTOperation op2)
        {
            var transformed = new OTOperation
            {
                Type = op1.Type,
                Position = op1.Position,
                Text = op1.Text,
                UserId = op1.UserId,
                Timestamp = op1.Timestamp
            };

            if (op1.Type == "INSERT"
                && op2.Type == "INSERT")
            {
                // If op2 inserted before op1 position
                // shift op1 position right
                if (op2.Position <= op1.Position)
                    transformed.Position +=
                        op2.Text.Length;
            }
            else if (op1.Type == "INSERT"
                     && op2.Type == "DELETE")
            {
                // If op2 deleted before op1 position
                // shift op1 position left
                if (op2.Position < op1.Position)
                    transformed.Position =
                        Math.Max(op2.Position,
                            op1.Position - op2.Text.Length);
            }
            else if (op1.Type == "DELETE"
                     && op2.Type == "INSERT")
            {
                // If op2 inserted before op1 position
                // shift op1 position right
                if (op2.Position <= op1.Position)
                    transformed.Position +=
                        op2.Text.Length;
            }
            else if (op1.Type == "DELETE"
                     && op2.Type == "DELETE")
            {
                // If op2 deleted before op1 position
                // shift op1 position left
                if (op2.Position < op1.Position)
                    transformed.Position =
                        Math.Max(op2.Position,
                            op1.Position - op2.Text.Length);
            }

            return transformed;
        }

        // Apply operation to document string
        public string Apply(
            string document, OTOperation op)
        {
            try
            {
                if (op.Type == "INSERT")
                {
                    var pos = Math.Clamp(
                        op.Position, 0, document.Length);
                    return document.Insert(pos, op.Text);
                }
                else if (op.Type == "DELETE")
                {
                    var pos = Math.Clamp(
                        op.Position, 0, document.Length);
                    var len = Math.Min(
                        op.Text.Length,
                        document.Length - pos);
                    if (len <= 0) return document;
                    return document.Remove(pos, len);
                }
            }
            catch
            {
                // If OT fails return document unchanged
            }

            return document;
        }
    }
}