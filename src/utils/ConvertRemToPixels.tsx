/**
 * Converts a rem value to a number of pixels for the current screen size.
 * @param rem The rem value to convert.
 */
export function convertRemToPixels(rem: string | number) {
	if (!rem) {
		rem = "1rem";
	}

	if (typeof rem === "string") {
		if (rem.indexOf("rem") >= 0) {
			rem = rem.substring(0, rem.length - 3);
			rem = parseFloat(rem);
		} else {
			rem = parseFloat(rem);
		}
	}

	const pixels =
		rem * parseFloat(getComputedStyle(document.documentElement).fontSize);

	return Math.round(pixels);
}
