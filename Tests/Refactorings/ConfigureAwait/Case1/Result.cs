// ConfigureAwait(false)
using System;
using System.Threading.Tasks;

class Source {
	public async Task Do() {
		await Task.Delay(1000).ConfigureAwait(false);
	}
}