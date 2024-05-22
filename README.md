# TDV_EDJD_PJ2

- Astroidz Gaming

# Introdução:

Este projeto teve por base a criação de um jogo com base no framework Monogame.

# Trabalho realizado por:

- 29579 - Roberto Alvarenga

- 29848 - André Azevedo

- 29580 - João Carvalho

# Sobre o Jogo:

- Este jogo ocorre em mundo aberto, onde a câmara segue o player, e o objetivo é tentar sobreviver o maior tempo possível e destruir os inimigos para obter score.

- Para jogar usa-se as teclas (awsd) e o rato para orientar a direção do player e o botãonesquerdo para disparar.   


# Sobre o código:

- **Classes:**

	- Game1.cs: Classe base da framework monogame e principal responsável pela jogabiliade e pelo player. É responsável por fazer o load content para gerar os asteroides e inimigos aleatóriamente e controlo e implementação do player, aqui usamos os métodos implementados nas outras classes. Também é aqui que se faz todo o draw. É responsável por método de debug e colisão que são usadas em outras partes do código. 
	
	- Astroid.cs: Nesta classe temos os astereoides, aqui é verificada a lógica de movimento e de colisão entre todos os objetos e asteroídes.
	
	- Bullet.cs: Nesta classe temos a crição e desenho das balas tendo em conta a posição de que a dispara.
	
	- Enemy.cs: Nesta classe temos a crição das naves inimigas, e é responsável por verificar as suas colisões e também o disparo de balas em direção ao player, e consante estes vãos endo destruidos novos são gerados.
	

- **Em falta**
	
	- Está implementado camara e os inimigos seguem o player, os asteroides podem ser destruídos, mas em caso de chocarem com o inimigo ou o player estes perdem vida.  
	
	- Não está implementado todos os sprites, menu, score e fim do jogo, sobre o fim do jogo ao chegar a zero a vida do player o jogo continua.

# Conclusão:

- Este projeto foi desafiante, uma vez que a framework obriga a pensar com cuidado na sua implementação, pois não existe uma base como em outras frameworks do tipo unity. 

- Como melhorias poderia ser implemantado as já identificadas anteriormente, bem como incluir som.