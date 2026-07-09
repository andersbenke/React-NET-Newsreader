function App() {
	function handleNavClick(e) {
		setActiveCategory(e.target.innerHTML);

		const data = {
			message: e.target.innerHTML
		};
		const event = new CustomEvent("nav-link-click", {
			detail: data
		});
		window.dispatchEvent(event);
	}

	let [activeCategory, setActiveCategory] = React.useState("top");
	return (<Nav navItems={settings_nav_items} activeCategory={activeCategory} clickHandler={handleNavClick} />);
}