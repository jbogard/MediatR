@using System.Net.Http.Headers
@model Dictionary<MediaTypeHeaderValue, object>

@{
    // Group the samples into a single tab if they are the same.
    Dictionary<string, object> samples = Model.GroupBy(pair => pair.Value).ToDictionary(
        pair => String.Join(", ", pair.Select(m => m.Key.ToString()).ToArray()), 
        pair => pair.Key);
    var mediaTypes = samples.Keys;
}
<div>
    @foreach (var mediaType in mediaTypes)
    {
        <h4 class="sample-header">@mediaType</h4>
        <div class="sample-content">
            <span><b>Sample:</b></span>
            @{
                var sample = samples[mediaType];
                if (sample == null)
                {
                    <p>Sample not available.</p>
                }
                else
                {
                    @Html.DisplayFor(s => sample);
                }
            }
        </div>
    }
</div>