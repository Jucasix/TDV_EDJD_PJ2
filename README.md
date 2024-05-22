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


# Sobre o código:

- **Classes:**

	- Game1.cs: Classe base da framework monogame e principal responsável pela jogabiliade e pelo player.
	
	- Astroid.cs: Nesta classe temos a crição e desenho dos astereoides de forma aleatória e até uma quantiade fixa para não encher o mapa de asteroides, os asteroides podem ser destruidos mas novos serão gerados.
	
	- Bullet.cs: Nesta classe temos a crição e desenho das balas tendo em conta a posição de que a dispara.
	
	- Enemy.cs: Nesta classe temos a crição das naves inimigas, e é responsável por verificar as suas colisões e também o disparo de balas em direção ao player, e consante estes vãos endo destruidos novos são gerados.
	

- **Comentários**
	
	- Está implementado camara e os inimigos seguem o player, os asteroides podem ser destruídos, mas em caso de chocarem com o inimigo ou o player estes perdem vida.  
	
	- Não está implementado score nem fim do jogo, ao chegar a zero a vida do player o jogo continua.

# Conclusão:

- Este projeto foi desafiante, uma vez que a framework obriga a uma implementação total, pois não existe uma base como em outras frameworks do tipo unity. 

 