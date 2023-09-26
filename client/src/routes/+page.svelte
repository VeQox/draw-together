<script lang="ts">
	import { onMount } from "svelte";

	//let canvas: HTMLCanvasElement;
	let webSocket: WebSocket;

	let position = { x: 0, y: 0 };

	let positions: {
		position: typeof position;
		id: string;
	}[] = [];
	$: positions;


	onMount(() => {
		//const ctx = canvas.getContext('2d');
		//if(!ctx) return;

		webSocket = new WebSocket("ws://localhost:5143/ws/canvas");
		webSocket.onopen = () => {
			console.log("WebSocket opened");
		}
		webSocket.onmessage = (e) => {
			const data = JSON.parse(e.data);

			if(data.event === 0) {
				let tmp = positions.find((p) => p.id === data.id);
				if(tmp) tmp.position = data.position;
				else positions.push(data);
				positions = positions;
			}
		};
		webSocket.onclose = () => {
			positions = [];
		};


		document.addEventListener("mousemove", (e) => {
			position.x = e.clientX;
			position.y = e.clientY;

			webSocket?.send(JSON.stringify({
				event: 0,
				position: position
			}));
		});
	});
</script>

<!--<canvas bind:this={canvas} style="w-full h-full" width="1920" height="1080" />-->


{#each positions as p}
	<div style="position: absolute; left: {p.position.x - 5}px; top: {p.position.y - 5}px; width: 10px; height: 10px; background-color: red; border-radius: 50%;"></div>
{/each}
